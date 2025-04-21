using ABB.Robotics.Math;
using ABB.Robotics.RobotStudio;
using ABB.Robotics.RobotStudio.Environment;
using ABB.Robotics.RobotStudio.Stations;
using ABB.Robotics.RobotStudio.Stations.Forms;
using ABB.Robotics.RobotStudio.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;


namespace Assistant
{
    public class Class1
    {
        public static void AddinMain()
        {
            Project.UndoContext.BeginUndoStep("Add Buttons");
            try
            {
                var ribbonTab = new RibbonTab("AI-Tab", "AI Assistant");
                UIEnvironment.RibbonTabs.Add(ribbonTab);
                UIEnvironment.ActiveRibbonTab = ribbonTab;

                var ribbonGroup = new RibbonGroup("MyButtons", "");

                var buttonFirst = CreateButton("MyFirstButton", "Ask AI", "Ask the AI-Assistant AI, trained on ABB Robotstudio documentation, a question about the software or how to use it.", "ai-icon.png", Ask_ChatGPT);
                var buttonSecond = CreateButton("MySecondButton", "Tutorials", "Click to view official ABB Tutorials in your browser", "tutorial-icon.png", Open_Tutorials);

                ribbonGroup.Controls.Add(buttonFirst);
                ribbonGroup.Controls.Add(new CommandBarSeparator());
                ribbonGroup.Controls.Add(buttonSecond);
                ribbonGroup.SetControlLayout(buttonFirst, RibbonControlLayout.Large);
                ribbonGroup.SetControlLayout(buttonSecond, RibbonControlLayout.Large);

                ribbonTab.Groups.Add(ribbonGroup);
            }
            catch
            {
                Project.UndoContext.CancelUndoStep(CancelUndoStepType.Rollback);
                throw;
            }
            finally
            {
                Project.UndoContext.EndUndoStep();
            }
        }

        /// <summary>
        /// creates an ABB RobotStudio CommandBarButton instance using the provided parameters
        /// </summary>
        internal static CommandBarButton CreateButton(string id, string label, string helpText, string iconFileName, ExecuteCommandEventHandler eventHandler)
        {
            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string projectDir = Directory.GetParent(assemblyDir)?.Parent?.FullName;
            string iconPath = Path.Combine(projectDir, "Properties", "icons", iconFileName);

            var button = new CommandBarButton(id, label)
            {
                HelpText = helpText,
                Image = Image.FromFile(iconPath),
                DefaultEnabled = true
            };
            button.ExecuteCommand += eventHandler;
            return button;
        }

        /// <summary>
        /// generates a window containing an upper and lower textbox. 
        /// Text can be entered into the upper textbox and on newline press will be appended to the lower textbox where a response from the LLM will also be generated and appended.
        /// The lower textbox cannot be directly altered by the user.
        /// </summary>
        internal static void Ask_ChatGPT(object sender, ExecuteCommandEventArgs e)
        {
            try
            {
                var panel = new Panel
                {
                    Dock = DockStyle.Fill
                };
                var upperTextBox = new RichTextBox
                {
                    Font = new Font("Arial", 12, FontStyle.Regular),
                    Dock = DockStyle.Top,
                };
                var lowerTextBox = new RichTextBox
                {
                    Font = new Font("Arial", 12, FontStyle.Regular),
                    Multiline = true,
                    ScrollBars = RichTextBoxScrollBars.Vertical,
                    WordWrap = true,
                    DetectUrls = true,
                    Dock = DockStyle.Fill, // Fill remaining space
                    ReadOnly = true // Make it read-only
                };

                upperTextBox.KeyDown += async (s, args) =>
                {
                    // If the user starts typing for the first time, remove placeholder
                    if (upperTextBox.Text == "Type your question here..." && args.KeyCode != Keys.Enter)
                    {
                        upperTextBox.Text = "";
                        upperTextBox.ForeColor = Color.Black;
                    }

                    if (args.KeyCode == Keys.Enter)
                    {
                        args.SuppressKeyPress = true; // Prevents the default newline behavior
                        args.Handled = true;

                        if (!string.IsNullOrWhiteSpace(upperTextBox.Text))
                        {
                            string userIn = upperTextBox.Text.Trim();
                            lowerTextBox.AppendText("User: " + userIn + "\n\n");

                            // Resetting the upper text box for new input
                            upperTextBox.Clear();
                            upperTextBox.Text = "Type your question here...";
                            upperTextBox.ForeColor = Color.Gray;

                            lowerTextBox.SelectionIndent = 60; // Indent AI response

                            // making the actual API call to ChatGPT
                            using(HttpClient client = new HttpClient())
                            {
                                string apiKey = Environment.GetEnvironmentVariable("chatgpt_api_key");
                                string endpoint = Environment.GetEnvironmentVariable("chatgpt_endpoint");
                                string deploymentID = Environment.GetEnvironmentVariable("chatgpt_deployment_id");
                                string requestUri = $"{endpoint}/openai/deployments/{deploymentID}/chat/completions?api-version=2024-08-01-preview";

                                client.DefaultRequestHeaders.Add("api-key", apiKey);
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                // constructing the system string content
                                string[] metadata = get_metadata();
                                string robotSubString = $"You are working with a {(metadata[1] ?? "generic ABB")} robot";
                                robotSubString += metadata[2] == null ? " which is not equipped with a tool." : $" equipped with a {metadata[2]} tool.";
                                string sysContent = $"You are an AI assistant working inside ABB's RobotStudio version {metadata[0]}. {robotSubString}. {metadata[3]}";

                                // creating the messages to be sent to the API and serializing them into a format that the API can read
                                var requestBody = new
                                {
                                    messages = new[]
                                    {
                                        new { role = "system", content = sysContent },
                                        new { role = "user", content = userIn }
                                    }
                                };
                                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                                try
                                {
                                    HttpResponseMessage response = await client.PostAsync(requestUri, content);
                                    response.EnsureSuccessStatusCode();

                                    string aiMessageJson = await response.Content.ReadAsStringAsync();

                                    var responseJson = JsonSerializer.Deserialize<JsonElement>(aiMessageJson);

                                    // you have to do this, or it will display as a json and not a string
                                    string aiResponse = responseJson
                                        .GetProperty("choices")[0]
                                        .GetProperty("message")
                                        .GetProperty("content")
                                        .GetString();

                                    lowerTextBox.AppendText($"AI: {aiResponse}\n\n");
                                }
                                catch (Exception ex)
                                {
                                    lowerTextBox.AppendText($"AI: Failed to get response. Error: {ex.Message}\n\n");
                                }
                            }

                            lowerTextBox.SelectionIndent = 0; // Return to normal indentation
                        }
                    }
                };
                upperTextBox.LostFocus += (s, args) =>
                {// Handle focus lost (restore placeholder)
                    if (string.IsNullOrWhiteSpace(upperTextBox.Text))
                    {
                        upperTextBox.Text = "Type your question here...";
                        upperTextBox.ForeColor = Color.Gray; // Placeholder color
                    }
                };
                lowerTextBox.LinkClicked += (s, args) =>
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(args.LinkText) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to open link: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                panel.Controls.Add(lowerTextBox);
                panel.Controls.Add(upperTextBox);

                var window = new DocumentWindow(Guid.NewGuid(), panel, "Ask AI");

                UIEnvironment.Windows.Add(window);

            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine("Version is null. Exception Thrown: {0}", ex.Message);
            }
        }

        /// <summary>
        /// helper function for the tutorials button. Opens a browser window containing the officially ABB video tutorials.
        /// </summary>
        internal static void Open_Tutorials(object sender, ExecuteCommandEventArgs e)
        {
            Process.Start("https://new.abb.com/products/robotics/software-and-digital/robotstudio/tutorials");
        }

        /// <summary>
        /// Gets metadata from the active project station.
        /// </summary>
        /// <returns>
        /// A string array containing: [RobotStudio Version, Robot Model, Tool Name, Is Active Status].
        /// </returns>
        internal static string[] get_metadata()
        {
            string version = RobotStudioAPI.Version.ToString();
            string robotModel = null;
            string toolName = null;
            string isActive = "Assume the robot is inactive";
            if (Project.ActiveProject != null && Station.ActiveStation != null)
            {
                foreach (GraphicComponent comp in Station.ActiveStation.GraphicComponents)
                {
                    if (comp is Mechanism robot)
                    {
                        // Perform actions on the robot
                        log($"Found Robot: {robot.Name} [{robot.ModelName}]");
                        robotModel = robot.ModelName;

                        if (robot.NumActiveJoints == 0)
                        {
                            log("Robot is available.");
                            isActive = "The robot is not active.";
                        }
                        else
                        {
                            log($"Robot is currently in use. [{robot.NumActiveJoints} active joints]");
                            isActive = "The robot is currently active.";
                        }
                    }
                    if (comp is Mechanism tool)
                    {
                        log($"Found Tool: {tool.Name}");
                        toolName = tool.Name;
                    }
                }
            }
            return [version, robotModel, toolName, isActive];
        }

        /// <summary>
        /// logs the input string to the ABB RobotStudio console
        /// </summary>
        internal static void log(string message)
        {
            Logger.AddMessage(new LogMessage(message));
        }

    }
}

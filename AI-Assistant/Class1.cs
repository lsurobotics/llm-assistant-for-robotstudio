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

        private static CommandBarButton CreateButton(string id, string label, string helpText, string iconFileName, ExecuteCommandEventHandler eventHandler)
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

        private static void Ask_ChatGPT(object sender, ExecuteCommandEventArgs e)
        {
            try
            {
                /* var licenses = LicenseValidator.GetInstalledLicenses();
                log("Currently Installed Licences:");
                foreach (var license in licenses)
                {
                    log($"{license.VendorInfo} Version {license.Version} [isValid: {license.IsValid}]");
                } */

                var textBox = new RichTextBox
                {
                    Font = new Font("Arial", 14, FontStyle.Regular),
                    Multiline = true,
                    ScrollBars = RichTextBoxScrollBars.Vertical,
                    WordWrap = true,
                    DetectUrls = true
                };
                textBox.KeyDown += myTextBox_KeyDown;
                textBox.LinkClicked += Mytextbox_LinkClicked;

                var window = new DocumentWindow(Guid.NewGuid(), textBox, "Ask AI");
                UIEnvironment.Windows.Add(window);
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine("Version is null. Exception Thrown: {0}", ex.Message);
            }
        }

        private static void Open_Tutorials(object sender, ExecuteCommandEventArgs e)
        {
            Process.Start("https://new.abb.com/products/robotics/software-and-digital/robotstudio/tutorials");
        }

        private static void log(string message)
        {
            Logger.AddMessage(new LogMessage(message));
        }

        private static void myTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (Window.ActiveWindow.Control is RichTextBox textBox)
                {
                    e.Handled = true;
                    string lastLine = textBox.Text.Split('\n').LastOrDefault()?.Trim();
                    if (!string.IsNullOrWhiteSpace(lastLine))
                    {
                        textBox.AppendText("\n\n");

                        textBox.SelectionIndent = 40;
                        textBox.Font = new Font("Arial", 14, FontStyle.Regular);

                        textBox.SelectedText = "This is a placeholder response from the AI-Assistant. Replace this with actual AI response logic.\n";

                        textBox.SelectionIndent = 0;
                        textBox.SelectionHangingIndent = 0;

                        textBox.AppendText("\n");

                    }
                }
            }
        }

        private static void Mytextbox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.LinkText) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open link: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}

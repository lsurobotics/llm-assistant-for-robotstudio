## Design Document

### Technologies / Languages
- Custom ChatGPT wrapper for RobotStudio
- Visual C#
- Note; recommended development environment is Visual Studio

### General Description
The addin should be a simple ChatGPT wrapper designed for use with RobotStudio. Users should be able to select a text entry field 
within RobotStudio, type in a question and recieve an answer in the form of a text explanation from the LLM. The answers given by
the LLM should ideally be as concise and clear as possible, using as little technical lingo as possible.

### Target Audience
The target audience for this application are users who have little to no experience using ABB's RobotStudio software.

### Use Cases
- As a Mechanical Engineer in the Robotics Club, I want to be able to quickly write some basic code so that I can test if the construction of my ABB robot is correct and it can perform its tasks as intened
- As a new hire at a manufacturing facility, I want to be able to get up to speed on the basics of RobotStudio, so that I can effectively manage the facility's robots
- As a RobotStudio marketer, I want to be able to show the ease of use and user-friendliness of our software, so that I can convince potential customers to use our product

### Development Environment Setup
1. Install RobotStudio (this will be needed to test the addin)
2. Install Visual Studio (this is the recommended development environment for RobotStudio Addins)
3. Install Visual C# if it is not already present on your machine
4. Install the RobotStudio SDK
    - This can be downloaded at the following url: https://developercenter.robotstudio.com/robotstudio-sdk/download
    - select the most recent version and follow the install guide
5. Open Visual Studio and select new project and search for "RobotStudio <version> Empty Add-in
    - If the templates do not appear, you may need to manually copy the folder containing the template zip-files into:
      ../Visual Studio <version>/Templates/ProjectTemplates/Visual Basic

### Running a Custom RobotStudio Add-In
To run a RobotStudio add-in you have created there are 3 steps
1. Execute the "build solution" command in Visual Studio, this will generate a .dll file
    - To do this right-click the .sln file and select the build option in the dropdown menu
2. Open the .rsaddin file and edit the <Path> [FilePath] </Path> to point to the newly generated .dll file
3. Copy the .rsadding file from the directory in the RobotStudio add-in folder
    - According to RobotStudio's developer center this is located in C:\Program Files (x86)\ABB\RobotStudio version\Bin\Addins by default
5. Finally, open RobotStudio, navigate to the Add-Ins menu, right click your add-in and select "Load Add-In" from the dropdown menu

### Feature Planning
- [x] - Collapsable Window in main RobotStudio development environment
- [x] - Text Entry Field
- [x] - Text Output Field
- [x] - Button that links directly to documentation for RobotStudio
- [ ] - Large Language Model Trained on RobotStudio Documentation
- [ ] - Output filtering software to ensure that text generated by the LLM is not overly wordy or complex
- [ ] - (Stretch Goal) Ability for the LLM to link relevant documentation if a user needs more detail on a particular question

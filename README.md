# UnityFaceMeshTracker
**DESCRIPTION**
-------------------------------------------------------------------------------------------
+ Beginner Face-Tracking Project featuring detection of all 478 landmarks, Iris Tracking and Blinking Simulation, Frame Optimization, and Real Time Data Transfer to Unity, building upon a YouTube Tutorial and using AI Agents to assist with face-tracking in 3D space

**BRANCHES**
-------------------------------------------------------------------------------------------
+ **MAIN** - Contains The Face Landmark Python File, and the Assets Folder containing the Unity Scene and the Corresponding C# Scripts

**REQUIREMENTS**
-------------------------------------------------------------------------------------------
+ **PYTHON 3.10+**
+ **UNITY 6.3 LTS (RECOMMENDED)**
+ **OPENCV, MEDIAPIPE, NUMPY (pip install opencv mediapipe numpy)**
+ **C# SUPPORT FOR VSCODE**
+ **CAMERA/WEBCAM SUPPORT**

**SETUP**
--------------------------------------------------------------------------------------------
1. Clone Repository: git clone https://github.com/PancitGuy/UnityFaceMeshTracker.git
2. In repository, open **FaceMeshCode.py** in main branch
3. Install any dependencies prior to running the file (**see dependencies**)
4. If not already, download and install **Unity version 6.3** (Any other version could work)
5. In the same main branch, locate the Unity Scene by accessing the **Scenes** subfolder inside the **Assets** folder, and click on **SampleScene.unity**

**ACTIVATION AND NAVIAGTION INSTRUCTIONS**
---------------------------------------------------------------------------------------------
1. In **FaceMeshCode.py**, set up the video by passing "Videos/<video_name>.mp4" as the parameter inside **cap = cv2.VideoCapture()**, located on **line 11** (to access webcam, just pass 0 as the parameter)
2. Run FaceMeshCode.py. (Optional - For sound, uncomment **audio()** (located on lines **41** and **194**) and its function definition. Inside function, pass in "Videos/<audio_name>.wav" to hear the corresponding audio, but do note that with **winsound** it does not correspond with pause and play)
3. Launch Unity and access **SampleScene.unity** through the **Scenes** subfolder from Assets.
4. Press play to access face mesh tracking scripts.

**Credits and Assistance**
----------------------------------------------------------------------------------------------
+ Original Face Landmark Tutorial - [Murtaza's Workshop] (Link to Tutorial: https://www.youtube.com/watch?v=V9bzew8A1tc&t=1079s)
+ Videos Downloaded From Instagram:
  + @jacobhahmlive(https://www.instagram.com/jacobhahmlive?utm_source=ig_web_button_share_sheet&igsh=ZDNlZDc0MzIxNw==)
  + @jeslyn._.n(https://www.instagram.com/jeslyn._.n?utm_source=ig_web_button_share_sheet&igsh=ZDNlZDc0MzIxNw==)
+ Assistance from Microsoft Copilot

**AUTHOR'S NOTE**
-----------------------------------------------------------------------------------------------
+ Although AI Agents were used for conceptual assistance, extension guidance, and debugging support, all final decisions, modifications, and implementations were made by the original author. The use of AI was for educational purposes and to accelerate development during production of this project. Users of this code should read and understand the code before using it for educational projects, and should not be used for unauthorized and illegal activities.

**THE AUTHOR RECOGNIZES AI'S IMPACT ON INDUSTRIES, AND DISCLOSURE OF AI-ASSISTANCE IS INCLUDED FOR ETHICAL TRANSPARENCY.**

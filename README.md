# MATB-II_CollabVR
Adaptation on the MATB-II to a collaborative VR environment.

Netcode: Photon https://www.photonengine.com/pun

Using VR controller, the bindings of 4 actions are required with SteamVR: Teleport, GrabPinch, GrabGrip (boolean actions) and Stick (2d vector)

The recorded data can be found in the MATB-II-VR_Data/Logs_session_0 folder.

Whith two users, each can take the responsibility of two tasks.
We usually group the SYSMON task with the TRACK task and the COMM task with the RESMAN task.
Anyone can interact with the entire task battery, there're no restrictions.

# How to run the App
Execute one of the compiled versions of MATB-II-VR (VR or Desktop releases) or open the Unity project and play the VR or Desktop launcher scene (clone the repository).
Among the compiled versions, only the Desktop one has access to the application controls where both unity launchers will grants acess to these controls.
From the Desktop compiled version, the controls will be available after connection in a panel on the right side of the screen, only if the "Experimenter" box has been checked before connecting.
From any Unity launchers, the controls will be available after connection on the "UNITY_CONTROLLER" GameObject.

Choose an identifier and press "Start Experiment".

When a new user is connecting, his camera becomes the main one, to get back to your point of view, press X on a keyboard or press a rear trigger on a VR controller.

## Training
After every users are connected to the app, one of the ones with the application controls can launch a task from the left panel (training panel, can be masked by unchecking the "Training" box) by , the task launched will be repeated until manually stopped.

## Scenario
From the application controls, stop all trainings ("ALL Stop"), reset the battery ("ALL Reset").

Choose a scenario from a dropdown menu (it probably be preselected on "Training" or "DemoCNRS").

Press "Start Experiment" to launch the scenario.

## Experimental data visualizations
From the application controls, you can (de)activate experimental data visualisation.
Note that the aggregative boards need to be generated first.
Note that the feed back of self status may need to be activated twice to be displayed.

# Create a new scenario
*Currently, it's only possible from the Unity project. For futur work I know I have to make that easier and more*

## Create the scenario file
Navigate to ***Assets/Scripts/Scenarii*** in the project window.

Right-click inside the project window and select ***Create/ScriptableObjects/TASKPlanning***.

Select the created file and from Unity inspector, you can define the scenario duration and add task triggers inside the associated lists (don't mind the Workload task, it's not implemented).

By clicking on the little gear at the top right of the inspector window, you can call the sorting function to ensure that the tasks are correctly ordered and randomize the SYSMON parameters.

## Adding a scenario to the app
Open the ***Assets/Scripts/MATBII/TASKPlanner.cs*** script.

You'll have to add your scenario at three differents places:
- The Scenario enum
- The TASKPlanner attributes
- The TASKPlanner Start() method

Open the ***Assets/Prefabs/MATBII-SYSTEM*** prefab and navigate in the inspector to the TASKPlanner component to drag your script from the project window to the newly added scenario variant attribute.

(optional) You can also add the scenario to the desktop application control panel by openning the ***Assets/Resources/DesktopUser*** prefab and navigate in the hierarchy window to the ***DesktopUser/Camera/Canvas/ExperimenterControl/Scenario*** GameObject to add your Scenario to the dropdown options.

## Deployment *(WIP)*
As long as you run your scenario as the Photon master client, you might not have to rebuild the app.

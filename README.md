# Parcel Fabric Smooth Radius Add-In
To assist in parcel maintenance efforts to clean up coincident curves in a production parcel fabric, Esri Canada has developed the Smooth Radius Add-In. This Add-In streamlines the process of updating radius values for arcs, regenerating the parcels to correct the radials lines, and meaning the centre points so the curves stay coincident. 

#Instructions
Once the Add-In has been compiled and installed, use the following steps to configure the Add-In for use in ArcMap:

1.	Open ArcMap and navigate to the ‘Customize’ menu> Customize Mode> Commands Tab
2.	In the ‘Show Commands Containing’ entry box, type the word ‘Smooth’
3.	Under the Categories section, select ‘Parcel Fabric Add-Ins’
4.	Under the Commands section, drag the ‘Smooth Radius Trace Tool’ pink circle icon onto the Parcel Editor toolbar

  ![addtointerface](https://cloud.githubusercontent.com/assets/8808482/16619380/d553401a-435b-11e6-91ed-10021bd8a612.jpg)

Once the Smooth Radius Trace Tool has been added to the ArcMap interface, the steps below provide information on how to use the Add-In:

1.	Navigate to the Parcel Editor drop down> Start Editing
2.	Zoom to an area in the parcel fabric where you would like to smooth out the radius values
3.	Click on the ‘Smooth Radius Trace Tool’ and snap to the starting point of the first curve you want to update (a tip will appear on your screen indicating you have snapped to the point):

  ![snappoint](https://cloud.githubusercontent.com/assets/8808482/16619384/daaa7308-435b-11e6-9823-ed49174653ab.jpg)

4.	After snapping to the starting point, trace along all the curve segments you would like to update:
5.	Double click when you are done tracing – a dockable window called ‘Smooth Radius’ will appear, which you can dock anywhere of your choosing:

  ![window](https://cloud.githubusercontent.com/assets/8808482/16619388/dce66cf8-435b-11e6-89fe-722c3ada5894.jpg)

6.	The Smooth Radius window provides the following functionality:
  * The Smooth Radius window is automatically populated with the average radius value from all the segments you have traced; however, you can manually enter in the radius value that is recorded on the plan. 
  * The Segment Details window shows a list of all the segments that were traced, and records the original radius value and the related Centerpoint ID. 
  * You can right click on each segment in the Segment Details Window, where you will be presented with a context menu that allows you to either: Flash, Pan To, Zoom To, or Exclude. 
  * Segments you have traced that are not curves will be identified as warnings in the Segment Details window – if the user chooses not to exclude these straight segments, the Add-In will convert them to curves for you. 
  * Segments you have traced that have a radius value more than 5% off the median value will be identified as warnings in the Segment Details window. 

  ![options](https://cloud.githubusercontent.com/assets/8808482/16619382/d6c2c6e6-435b-11e6-8914-d755bc95aac8.jpg)

Note: You will need to save edits in order to save the work done within the edit session. 

# Known Issues

In rare cases, tracing a specific set of curves will result in a "Field Is Not Editable" error. We are investigating the problem, but currently, curves that return this error will need to be edited manually.  See the [issue](/../../issues/4) for more deatial.

If you encounter any other issues with the Add-In, please send a note to Sarah: ssibbett@esri.ca. The Add-In is delivered as-is, but we will do our best to investigate problems that have been reported. 

# Requirements

This Add-In requires ArcGIS Desktop 10.1 or higher, with a Standard or Advanced license. 

# Compiling Source Code

The Add-In source code will compile with:
  * ArcGIS 10.1 SDK or higher
  * .Net 3.5 or higher
  * Visual Studio 2010 or higher

# Licensing

You may not use this file except in compliance with the license agreement. 

Unless required by applicable law or agreed to in writing, Licensor provides the Work (and each Contributor provides its Contributions) on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied, including, without limitation, any warranties or conditions of TITLE, NON-INFRINGEMENT, MERCHANTABILITY, or FITNESS FOR A PARTICULAR PURPOSE. Licensee is solely responsible for determining the appropriateness of using or redistributing the Work and assume any risks associated with Licensee’s exercise of permissions under this License.

A copy of the license is available in the repository's [license.txt](https://github.com/EsriCanada/ParcelFabricSmoothRadius/blob/master/license.txt) file. 

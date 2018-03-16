# Generative City Semantic w/ Unity

Unity project for generative city in semantic image segmentation style.

## Installation
- Open Unity and create empty project.
- Dump contents of `/Assets` folder from this repo into `/Assets` folder of the empty project.

## Files & Folders
script | Contents
----------|---------
`City.cs` | city generator script
`Car.cs` | appearance and AI for moving objects (cars, bikes, people, etc.)
`Game.cs` | run everything
`MaterialManager.cs` | manage materials conforming to cityscape's color coding
`MeshMaker.cs` | procedural mesh generator
`Shape.cs` | util & math for 2D shapes
`SplitCam.cs` | split camera for stereo


dir name | contents
----------|---------
`Resources/LABELED_MATERIALS` | cityscape color coding as unity materials
`Standard Assets` | contains Unity's default FPS controller

# Square 9 Process Poster

This is an example project demonstrating how to create and post a GlobalCapture Process to a GlobalCapture API, written in C# on .NET Framework 4.6.1

## Prerequisites

**GlobalCapture 2.1.0.106**

**NewtonsoftJson 11.0.1**

## Getting Started

This project contains example code and inline comments documenting the full procedure of creating and posting a process to a GlobalCapture API. 

Additionally, The Square9.ProcessPoster assembly can be built and used in existing projects. To do so, follow these simple steps:

* Build the assembly, add it as a reference, and install its dependencies.
* Inside a **using** block, construct a new **ProcessPoster** object with a Capture API **url**, **username**, and **password**:

 ```
 using (var poster = new ProcessPoster(url, username, password))
 {
 	// post a process
 }
 ```
 
* Call the **PostProcess** method on the **ProcessPoster** object using a **workflowId**, **portalId**, and some **file information**:

 ```
 var process = poster.PostProcess(workflowId, portalId, filePath)
 // or...
 var process = poster.PostProcess(workflowId, portalId, fileBytes, fileName)
 ```

#### Enjoy!
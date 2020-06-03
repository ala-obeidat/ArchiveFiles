# ArchiveFiles
C# Windows Service used to compress and archive old files

### The main idea in this service:
Compress and archive files not longer access from checking directory to destination directory.

### Its advantages are as follows:
- It can be used for best utilize your storage without lossing data. 
- Moving old files(no longer used) from storage(or even folder) to another one.
- All working runs in backend service.
 
  <!---->
    <add key="" value="12"/>
    
    <!--File Last Access time in hours-->
    <add key="" value="0"/>
    
    <!--Source Directory to check old files-->
    <add key="" value="D:\Download"/>

    <!--Destination Directory to check old files-->
    <add key="DestinationDirectory" value="E:\Archive"/>
    
### Configuration
#### all configuration come from appSettings tag in App.config below:
 -  **Interval:**  Start Interval for service to start checking and working in hours
    - Integer value like "12"
 -  **LastAccessTime:** File Last Access time in hours to mark this file as old
    - Integer value like "6"
 -  **SourceDirectory:** Source Directory to check old files
    - string value like "D:\Download"
 -  **DestinationDirectory:** Destination Directory to add compress files here
     - string value like "F:\Backup"
  
## Note: 
Arvice file names will be in this format **dd-MM-yyyy.zip** *like "22-10-2020.zip"*

### Technology Used For Projects:
1. **Archive files** Windows Service

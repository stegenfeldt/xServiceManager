# xServiceManager

Complementary powershell script-based module to the ServiceManager module included in the System Center Service Manager products, 2019 and later.  
Perhaps it will work with earlier versions, but it's not tested and not supported.

Hopefully, this module will also be usable in both Windows Powershell and Powershell Core (on Windows), but it's not tested yet.  
At least, it isn't crashing when I run it in Powershell Core on Windows which both the ServiceManager module, and SMLets does.

## Naming and Code Convention

All functions exposed to the end-user of this module must follow the `Verb-xSCSMNoun` formula. If the function you write is public, it must have the `xSCSM` noun-prefix.
If you are unsure of allowed verbs, just run `Get-Verb`.

One function or cmdlet per file, and the file must have the same name as the exposed function/cmdlet name, with the .ps1/.cs extension. 

Scripts should be formatted following OTBS. Suggested settings for this workspace for vscode is included in the `.vscode` folder. These allow for a quick 'Format Document' on powershell scripts to make sure it's looking alright.

Stay away from bracket-less if-statements, they are easy to mess up in later updates. Ternary operators are fine, but should be used sparingly.

## Project structure

### C# based module code

All the C# based module code is in the `/src` directory.  
This is primarily where the module is build, mainly for performance reasons. 

### Powershell based module code

All the script based module code is in the `/xServiceManager` directory.  
This allows for quick development of new functions without having to delve into C#.

The `xServiceManager.psm1` file should generally not define functions and does not really need to be touched. It will scan the `Public` and `Private` directories and dot-source the files. Scripts within the `Public` folder will be exported as ModuleMembers (functions) to the end-user.

`/.vscode` contains a few helper configurations for working with the module in vscode.

`test` is where Pester tests are defined

`Private` is where you want to place *internal* functions. Helper functions that your functions may need but should be hidden from the end-user.  
Use one script-file per function, use the same name of the file as the function you're building (with .ps1 extension, ofcourse) and follow proper powershell naming conventions. 

`Public` contains all the functions that are exposed (exported) to the end-user using the module.  
One Script-file per function. Follow this project's naming convention as well as proper powershell naming. 

`scratchpad` is your playground, where you can test the functionality of the module without the constraints of pester tests.

### Docs

`docs` contains the markdown files that are used to generate the documentation for the module.  
This will later be configured to use DocFX to generate the documentation, but for now it's just markdown files.

## Module References

References to anything other than the standard `ServiceManager` module is strongly discouraged.  
The purpose is to extend that module with missing functionality in a way that doesn't break with a new version of Service Manager, or has to be installed with `-AllowClobber` to work, thus overwriting new core cmdlets.  
Yes, this paragraph is targeted at SMLets. I like you, you've helped me a lot, but you require me to jump through hoops because you decided to try and replace built-in cmdlets which has caused collisions with new versions of Service Manager.

Peaceful coexistance with the module provided by Microsoft is the goal.
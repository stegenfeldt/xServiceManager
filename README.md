# xServiceManager
Complementary powershell script-based module to the ServiceManager module included in the System Center Service Manager products, 2016 and later.

## Naming and Code Convention

All functions exposed to the end-user of this module must follow the `Verb-xSCSMNoun` formula. If the function you write is public, it must have the `xSCSM` noun-prefix.
If you are unsure of allowed verbs, just run `Get-Verb`.

One function per script file, and the file must have the same name as the function, with the .ps1 extension. 

Scripts should be formatted following OTBS. Suggested settings for this workspace for vscode is included in the `.vscode` folder. These allow for a quick 'Format Document' to make sure it's looking alright.

## Project structure

All the C# based module code is in the `src` directory.

All the script based module code is in the `xServiceManager` directory.  

The `xServiceManager.psm1` file should generally not define functions and does not really need to be touched. It will scan the `Public` and `Private` directories and dot-source the files. Scripts within the `Public` folder will be exported as ModuleMembers to the end-user.

`.vscode` contains a few helper configurations for working with the module in vscode.

`test` is where Pester tests are defined

`Private` is where you want to place *internal* functions. Helper functions that your cmdlets may need but should be hidden from the end-user.  
Use one script-file per function, use the same name of the file as the function you're building (with .ps1 extension, ofcourse) and follow proper powershell naming conventions. 

`Public` contains all the functions that are exposed (exported) to the end-user using the module.  
One Script-file per function. Follow this project's naming convention as well as proper powershell naming. 

`scratchpad` is your playground, where you can test the functionality of the module without the constraints of pester tests.

## Module References

References to anything other than the standard `ServiceManager` module is strongly discouraged. The purpose is to extend that module with missing functionality in a way that doesn't break with a new version of Service Manager, require recompiling a bunch of DLL files, or has to be installed with `-AllowClobber` to work.  
Yes, this paragraph is targeted at SMLets. I like you, you've helped me a lot, but you require me to jump through hoops because you decided to try and replace built-in cmdlets. 

Peaceful coexistance with the module provided by Microsoft is the goal.
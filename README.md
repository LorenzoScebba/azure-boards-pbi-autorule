# azure-boards-pbi-autorule

### How to run in local env:

- Copy and paste the appsettings.sample.json file and rename it to appsettings.json
- Replace the Azure.Uri and Azure.Pat variables
- Edit the Rules as you like, or leave it like it is already
- Run 
- ??
- Profit

### How to run in docker env (hopefully, im not testing this :P):

Use the following env variables:

```json
{
  "Logging__LogLevel__Default": "Information",
  "Logging__LogLevel__Microsoft": "Warning",
  "Logging__LogLevel__Microsoft.Hosting.Lifetime": "Information",
  "Azure__Pat": "****************************************************",
  "Azure__Uri": "https://dev.azure.com/*****",
  "Rules__Type": "Task",
  
  "Rules__Rules__0__IfChildState": "To Do",
  "Rules__Rules__0__NotParentStates__0": "Done",
  "Rules__Rules__0__NotParentStates__1": "Removed",
  "Rules__Rules__0__SetParentStateTo": "New",
  "Rules__Rules__0__AllChildren": true,

  "Rules__Rules__1__IfChildState": "In Progress",
  "Rules__Rules__1__NotParentStates__0": "Done",
  "Rules__Rules__1__NotParentStates__1": "Removed",
  "Rules__Rules__1__SetParentStateTo": "Committed",
  "Rules__Rules__1__AllChildren": false,

  "Rules__Rules__2__IfChildState": "Done",
  "Rules__Rules__2__NotParentStates__0": "Removed",
  "Rules__Rules__2__SetParentStateTo": "Done",
  "Rules__Rules__2__AllChildren": true
}
```
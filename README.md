# azure-boards-pbi-autorule

### How to run in local env:

- Copy and paste the appsettings.sample.json file and rename it to appsettings.json
- Replace the Azure.Uri and Azure.Pat variables
- Edit the Rules as you like, or leave it like it is already
- Run 
- ??
- Profit

### How to run in docker env (hopefully, im not testing this :P):

Use the env variables declared in the env.list file (env.example.list):

*lorenzoscebba.azurecr.io/azure-boards-pbi-autorule:latest is a private repo, build your own image ;)*

```bash
docker run --env-file env.list lorenzoscebba.azurecr.io/azure-boards-pbi-autorule:latest
```

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

Or on azure bulk edit: 

```json
[
  {
    "name": "Logging__LogLevel__Default",
    "value": "Information",
    "slotSetting": false
  },
  {
    "name": "Logging__LogLevel__Microsoft",
    "value": "Warning",
    "slotSetting": false
  },
  {
    "name": "Logging__LogLevel__Microsoft.Hosting.Lifetime",
    "value": "Information",
    "slotSetting": false
  },
  {
    "name": "Azure__Pat",
    "value": "****************************************************",
    "slotSetting": false
  },
  {
    "name": "Azure__Uri",
    "value": "https://dev.azure.com/*****",
    "slotSetting": false
  },
  {
    "name": "Rules__Type",
    "value": "Task",
    "slotSetting": false
  },
  {
    "name": "Rules__Rules__0__IfChildState",
    "value": "To Do",
    "slotSetting": false
  },
  {
    "name": "Rules__Rules__0__NotParentStates__0",
    "value": "Done",
    "slotSetting": false
  },
  {
    "name": "Rules__Rules__0__NotParentStates__1",
    "value": "Removed",
    "slotSetting": false
  },
  {
    "name": "Rules__Rules__0__SetParentStateTo",
    "value": "New",
    "slotSetting": false
  },
  {
    "name": "Rules__Rules__0__AllChildren",
    "value": "true",
    "slotSetting": false
  },
  {
    "name": "Rules__Rules__1__IfChildState",
    "value": "In Progress",
    "slotSetting": false
  },
  {
    "name": "Rules__Rules__1__NotParentStates__0",
    "value": "Done",
    "slotSetting": false
  },
  {
    "name": "Rules__Rules__1__NotParentStates__1",
    "value": "Removed",
    "slotSetting": false
  },
  {
    "name": "Rules__Rules__1__SetParentStateTo",
    "value": "Committed",
    "slotSetting": false
  },
  {
    "name": "Rules__Rules__1__AllChildren",
    "value": "false",
    "slotSetting": false
  },
  {
    "name": "Rules__Rules__2__IfChildState",
    "value": "Done",
    "slotSetting": false
  },
  {
    "name": "Rules__Rules__2__NotParentStates__0",
    "value": "Removed",
    "slotSetting": false
  },
  {
    "name": "Rules__Rules__2__SetParentStateTo",
    "value": "Done",
    "slotSetting": false
  },
  {
    "name": "Rules__Rules__2__AllChildren",
    "value": "true",
    "slotSetting": false
  }
]
```
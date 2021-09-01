[![.NET](https://github.com/LorenzoScebba/azure-boards-pbi-autorule/actions/workflows/dotnet-test.yml/badge.svg)](https://github.com/LorenzoScebba/azure-boards-pbi-autorule/actions/workflows/dotnet-test.yml)
[![Docker Image CI](https://github.com/LorenzoScebba/azure-boards-pbi-autorule/actions/workflows/docker-image.yml/badge.svg)](https://github.com/LorenzoScebba/azure-boards-pbi-autorule/actions/workflows/docker-image.yml)
[![Docker hub](https://res.cloudinary.com/dsb3vmg4x/image/upload/b_rgb:dae8fd,c_fit,h_20,w_100,c_pad/v1629815565/azure-boards-pbi-autorule/docker.png)](https://hub.docker.com/r/lorenzoscebba/azure-boards-pbi-autorule)

# Wait what is this?

This ~~demonic creation~~ project should help users of azure boards to automatically move a PBI parent to a particular
state based on the pbi state updates.

Each rule can be customized and consists of the following variables:

```json
{
  "Type": "Task",
  "Rules": [
    {
      "IfState": "In Progress",
      "NotParentStates": [
        "Done",
        "Removed"
      ],
      "SetParentStateTo": "Committed",
      "All": false
    }
  ]
}
```

The above rule is triggered each time a **Task** moves from any state to **In Progress** (`IfState`), the rule also
checks that the parent state is not **Done** or **Removed** (`NotParentState`) and if that's the case it modifies the
parent state to **Committed** (`SetParentStateTo`). For this rule to work it is not necessary that all childrens are
**In Progress** (`All`)

## How to configure it

<details>
  <summary>Expand</summary>

- Create a new Service Hook in azure devops of type `Web Hook`
- The trigger should be `Work item updated`
    - Area Path: `[Any]` or a specific area path based on your needs
    - Work item type: `[Any]`
    - Tag: Leave it empty or fill it based on your needs
    - Field: `State`
- Url: `https://<URL_OF_SERVICE>/api/receive`

</details>

## How to run it

### How to run in local env:

<details>
  <summary>Expand</summary>

- Copy and paste the `appsettings.sample.json` file and rename it to `appsettings.json`
- Replace the `Azure.Uri` and `Azure.Pat` variables
- Edit the Rules as you like, or leave it like it is already
- Run
- ??
- Profit

</details>

### How to run in docker env:

<details>
<summary>Expand</summary>

Duplicate the file env.example.list and rename it to env.list, fill out the Azure Vars and run:

```bash
docker build -t azure-boards-pbi-autorule:latest .

docker run --env-file env.list -p 5000:80 azure-boards-pbi-autorule:latest
```

<details>
  <summary>Reference variables</summary>

```json
{
  "Azure__Pat": "****************************************************",
  "Azure__Uri": "https://dev.azure.com/*****",
  "Rules__0__Type": "Task",
  "Rules__0__Rules__0__IfState": "To Do",
  "Rules__0__Rules__0__NotParentStates__0": "Done",
  "Rules__0__Rules__0__NotParentStates__1": "Removed",
  "Rules__0__Rules__0__SetParentStateTo": "New",
  "Rules__0__Rules__0__All": true,
  "Rules__0__Rules__1__IfState": "In Progress",
  "Rules__0__Rules__1__NotParentStates__0": "Done",
  "Rules__0__Rules__1__NotParentStates__1": "Removed",
  "Rules__0__Rules__1__SetParentStateTo": "Committed",
  "Rules__0__Rules__1__All": false,
  "Rules__0__Rules__2__IfState": "Done",
  "Rules__0__Rules__2__NotParentStates__0": "Removed",
  "Rules__0__Rules__2__SetParentStateTo": "Done",
  "Rules__0__Rules__2__All": true
}
```

</details>

<details>
  <summary>Reference Azure Web-App Variables</summary>

```json
[
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
    "name": "Rules__0__Type",
    "value": "Task",
    "slotSetting": false
  },
  {
    "name": "Rules__0__Rules__0__IfState",
    "value": "To Do",
    "slotSetting": false
  },
  {
    "name": "Rules__0__Rules__0__NotParentStates__0",
    "value": "Done",
    "slotSetting": false
  },
  {
    "name": "Rules__0__Rules__0__NotParentStates__1",
    "value": "Removed",
    "slotSetting": false
  },
  {
    "name": "Rules__0__Rules__0__SetParentStateTo",
    "value": "New",
    "slotSetting": false
  },
  {
    "name": "Rules__0__Rules__0__All",
    "value": "true",
    "slotSetting": false
  },
  {
    "name": "Rules__0__Rules__1__IfState",
    "value": "In Progress",
    "slotSetting": false
  },
  {
    "name": "Rules__0__Rules__1__NotParentStates__0",
    "value": "Done",
    "slotSetting": false
  },
  {
    "name": "Rules__0__Rules__1__NotParentStates__1",
    "value": "Removed",
    "slotSetting": false
  },
  {
    "name": "Rules__0__Rules__1__SetParentStateTo",
    "value": "Committed",
    "slotSetting": false
  },
  {
    "name": "Rules__0__Rules__1__All",
    "value": "false",
    "slotSetting": false
  },
  {
    "name": "Rules__0__Rules__2__IfState",
    "value": "Done",
    "slotSetting": false
  },
  {
    "name": "Rules__0__Rules__2__NotParentStates__0",
    "value": "Removed",
    "slotSetting": false
  },
  {
    "name": "Rules__0__Rules__2__SetParentStateTo",
    "value": "Done",
    "slotSetting": false
  },
  {
    "name": "Rules__0__Rules__2__All",
    "value": "true",
    "slotSetting": false
  }
]
```

</details>
</details>
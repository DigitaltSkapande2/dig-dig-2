

### --- Debugger Modules --- ###

Debugger modules are Prefabs you put in the scene and they do various different things to help with Debugging and Build Debugging

you can manage Debugger Modules through the DebugMenu



### --- For Scripting --- ###

--- Serialization 
A class defined with the [Debug] Attribute will be displayed in the Debug menu

# In said class...
Fields defined with the [DebugSerialized] Attribute will be Serialized and editable through the Debug menu

Note that the Serialization is limited to the Datatypes bool, string, float and int

--- NameSpace
All [Debug] Classes should be written to be for Debug Only... 
and those Classes should be defined within the Namespace Bob.Debugging.Modules

SimpleMapper - small library for mapping types easily.

To use the library, you need to install the nuget package {link}

For use in WebApi apps you need to add in Startup.cs single line in ConfigureServices func.
Like this - `services.AddSimpleMapper()`

In other cases U need to call `Mapper.Init()` and use `Mapper.Instance` for map types
Example you can find in project with tests

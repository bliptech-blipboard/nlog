# Blipboard Network Target

This package contains a simple derivation of NLog's `NetworkTarget` configuring sensible defaults for easy use with the [Blipboard event visualization](https://blipboard.io).

You can use it by adding the package:

```sh
dotnet add package Bliptech.Blipboard.NLog
```

and configuring the target in you `nlog.config`. The simplest configuration looks like this:

```xml
<nlog>
  <extensions>
    <add assembly="Bliptech.Blipboard.NLog"/>
  </extensions>
  
  <targets async="true">
    <target
      name="Blipboard"
      xsi:type="BlipboardNetwork"
      address="<endpoint-url>"
    />
  </targets>

  ...

</nlog>
```

## Customize labels, messages and details

You can configure the following attributes for more control over labels and details:

```xml
    <target
      name="Blipboard"
      xsi:type="BlipboardNetwork"
      address="<endpoint-url>"

      label="some-prefix:${logger:truncate=80}"
      message="${message} (${some-var})"
      details="${exception:format=tostring:maxInnerExceptionLevel=2:truncate=2000}"
    />
```

Make sure to include the truncation operators as Blipboard has a total ingestion limit of 4K and will reject longer request bodies.

## Message templates and collapsers

The target sends the raw message template as `msg_template`:

```c#
// Both those logs will appear in the same channel as
// msg_template will be the same in both cases
log.Info("The user {user} logged on", "bob");
log.Info("The user {user} logged on", "alice");
```

However, since this type of grouping is of lesser concern for other targets, it's common that expanded messages are passed to NLog and therefore the target can no longer tell if they should be grouped together:

```c#
// Bad: By default, all of these will appear in their own channel
//      and can thus spam your channel real-estate.
log.Info($"The user '{userName}' with id {userId} logged on");
```

To mitigate this, the `BlipboardNetwork` target adds a collapser feature that uses regexes to collapse the message further. With the following configuration, the above example will still only log to one channel:

```xml
    <target
        ...

        collapse="single-quotes,guids,numbers"
    />
```

You can see all pre-defined collapser definitions [here](https://github.com/bliptech-blipboard/nlog/tree/master/NLog/StandardCollapsePatterns.cs).

Especially numbers are almost always useful to collapse.

You can also add some custom ones globally with

```c#
Bliptech.Blipboard.NLog.CollapsePatternRegistry.Default.Add(someName, someRegex);
```

Note that using regexes has some performance impact.

## It's really just the `NetworkTarget`

All features of this target are implemented on top of NLog's existing features in the sense that each configuration of `BlipboardNetwork` could be replaced with an equivalent configuration of `Network`.

This includes the collapsers, which would then require those regexes to be defined in variables in the `nlog.config` and used in respective layouters.

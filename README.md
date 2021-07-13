# FromAToB

A simple library that will allow you to move data from one or many data sources to one or many destinations. This project is mostly created to increase my knowledge with [Rx](https://github.com/dotnet/reactive)).

How to use? (more examples comming soon)

```csharp
var source = Source
	.FromStream(memoryStream, (int)2048, 0)
	.And()
	.FromHttpGet("https://google.com", httpClient)
	.ToConsole(bytes => Encoding.UTF8.GetString(bytes))
	.Start(CancellationToken.None);
```

While this is still being built, the usability of this library will the main focus from here on out. This library is not meant to be used in anything serious.
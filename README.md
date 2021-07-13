# FromAToB (Moving data from A To B)

A simple library that will allow you to move data from one or many data sources to one or many destinations. This project is mostly created to increase my knowledge with [Rx](https://github.com/dotnet/reactive).

How to use? (more examples comming soon)

```csharp
 await Source
	.FromStream(memoryStream, 2048, 0) //buffer size and initial offset
	.And()
	.FromHttpGet("https://google.com", httpClient)
	.ToConsole(bytes => Encoding.UTF8.GetString(bytes)) //Entwined results from both stream and HTTP
	.Start(CancellationToken.None); //Executes the pipeline
```

While this is still being built, the usability of this library will the main focus from here on out. This library is not meant to be used in anything serious.
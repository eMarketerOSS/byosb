# Build your own service bus

One `UnicastBus.cs` file will contain the whole bus implementation. It is intended to be used as a drop-in file to enable unobtrusive messaging in .net apps.

### Feature target
* Routing
* Publish/Subscribe
* Request/Replay
* Error Handing
* Sagas
* Timeouts

## Installation

1. Yep, copy/paste `UnicastBus.cs` in your project
2. `Install-Package WindowsAzure.ServiceBus`

## Development

Set in the environment variables `shuttle-sb-connection` with azure service bus connection string.  

## Contributing

1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request :D

## License

[Mozilla Public License Version 2.0](https://www.mozilla.org/en-US/MPL/2.0/)
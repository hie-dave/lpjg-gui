using CommandLine;
using LpjGuess.Frontend;
using LpjGuess.Frontend.Config;
using LpjGuess.Frontend.DependencyInjection;

Options options = Options.Parse(args);
IServiceLocator locator = new ServiceLocator();
IApplication app = locator.GetService<IApplication>();
app.Run(options.Unparsed.ToArray());

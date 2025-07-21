using LpjGuess.Frontend;
using LpjGuess.Frontend.DependencyInjection;

IServiceLocator locator = new ServiceLocator();
IApplication app = locator.GetService<IApplication>();
app.Run(args);

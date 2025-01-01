using MapIt.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Forms;

namespace MapIt
{
    public class Bootstrapper
    {
        private readonly IServiceProvider _serviceProvider;

        public Bootstrapper()
        {
            // Initialisation des services
            _serviceProvider = ConfigureServices().BuildServiceProvider();
        }

        // Configuration des services
        protected virtual IServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddScoped<IAppInstanceManager, SingleInstanceAppManager>();
            services.AddScoped<IKeyMappingService, JsonKeyMappingService>();
            services.AddScoped<IKeyboardHookService, KeyboardHookService>();
            services.AddScoped<INotifyIconService, NotifyIconService>();

            return services;
        }

        public virtual void Run()
        {
            // Résolution des services
            var appInstanceManager = _serviceProvider.GetRequiredService<IAppInstanceManager>();
            var keyMappingService = _serviceProvider.GetRequiredService<IKeyMappingService>();
            var keyboardHookService = _serviceProvider.GetRequiredService<IKeyboardHookService>();
            var notifyIconService = _serviceProvider.GetRequiredService<INotifyIconService>();

            // Vérifier qu'il n'y a qu'une seule instance
            if (!appInstanceManager.CanExecuteNewInstance("MapIt"))
            {
                return;
            }

            // Charger les mappings et configurer le hook
            var keyMapping = keyMappingService.LoadKeyMappings();
            keyboardHookService.SetKeyMapping(keyMapping);

            // Démarrer l'écoute des touches
            keyboardHookService.StartListening();

            // Initialiser le NotifyIcon et le menu contextuel
            notifyIconService.Initialize();

            // Exécuter l'application
            Application.Run();
        }
    }
}
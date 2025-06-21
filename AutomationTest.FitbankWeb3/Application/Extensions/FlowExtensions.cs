using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using Microsoft.Playwright;
using Xunit.Abstractions;

namespace AutomationTest.FitbankWeb3.Application.Extensions
{
    public static class FlowExtensions
    {
        private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public static string GenerateRandomString(int length)
        {
            var result = new StringBuilder(length);
            var rng = RandomNumberGenerator.Create();
            var buffer = new byte[sizeof(uint)];

            for (int i = 0; i < length; i++)
            {
                rng.GetBytes(buffer);
                uint num = BitConverter.ToUInt32(buffer, 0);
                result.Append(_chars[(int)(num % _chars.Length)]);
            }

            return result.ToString();
        }
        /// <summary>
        /// Clikk a un elemento y espera a que aparezca el elemento de confirmación.Repite si no aparece en el límite de tiempo o si aparece el elemento de error.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="locatorClick"></param>
        /// <param name="locatorSuccess"></param>
        /// <param name="locatorError"></param>
        /// <param name="actionCoordinatorService">Servicio de coordinacion de click para test en paralelo</param>
        /// <param name="optionsWait"></param>
        /// <param name="outputHelper"></param>
        /// <param name="maxRetries"></param>
        /// <param name="optionsClick"></param>
        /// <param name="transitionBufferMs"></param>
        /// <returns></returns>
        public static async Task ClickAndWaitAsync(
            this IPage page,
            ILocator locatorClick,
            ILocator locatorSuccess,
            ILocator locatorError,
            IActionCoordinatorService actionCoordinatorService,
            LocatorWaitForOptions? optionsWait = null,
            ITestOutputHelper? outputHelper = null,
            int maxRetries = 6,
            LocatorClickOptions? optionsClick = null,
            int transitionBufferMs = 500)
        {
            // Asegurarse de que el ActionCoordinatorService está listo para usar
            using (var handle = actionCoordinatorService.CreateHandle())
            {
                await handle.WaitForTurnAsync();

                await ClickAndWaitAsync(page, locatorClick, locatorSuccess, locatorError, optionsWait, outputHelper, maxRetries, optionsClick, transitionBufferMs);
            }

            //try
            //    {
            //        await ClickAndWaitAsync(page, locatorClick, locatorSuccess, locatorError, optionsWait, outputHelper, maxRetries, optionsClick, transitionBufferMs);
            //    }
            //    finally
            //    {
            //        actionCoordinatorService.ReleaseTurn(); // Liberar el ActionCoordinatorService después de completar la acción
            //    }
        }
        /// <summary>
        ///Click a un elemento y espera a que aparezca el elemento de confirmación.Repite si no aparece en el límite de tiempo o si aparece el elemento de error.       
        /// </summary>
        /// <param name="page"></param>
        /// <param name="locatorClick"></param>
        /// <param name="locatorSuccess"></param>
        /// <param name="locatorError"></param>
        /// <param name="optionsWait"></param>
        /// <param name="outputHelper"></param>
        /// <param name="maxRetries"></param>
        /// <param name="optionsClick"></param>
        /// <param name="transitionBufferMs"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static async Task ClickAndWaitAsync(
            this IPage page,
            ILocator locatorClick,
            ILocator locatorSuccess,
            ILocator locatorError,
            LocatorWaitForOptions? optionsWait = null,
            ITestOutputHelper? outputHelper = null,
            int maxRetries = 6,
            LocatorClickOptions? optionsClick = null,
            int transitionBufferMs = 500)
        {
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                if (attempt > 0)
                    outputHelper?.WriteLine($"Reintento #{attempt}");

                // 1) Hacer click
                await locatorClick.ClickAsync(optionsClick);

                // 2) Pequeño buffer para que el UI se estabilice
                await Task.Delay(transitionBufferMs);

                // 3) Arrancar las tareas de espera, _después_ del click
                var successTask = locatorSuccess.WaitForElementSafeAsync(optionsWait);
                var errorTask = locatorError.WaitForElementSafeAsync(optionsWait);

                // 4) Esperar a que cualquiera termine
                var first = await Task.WhenAny(successTask, errorTask);
                var found = await first;

                // 5) Si la tarea que terminó primero devolvió 'false', significa que
                //    ese selector no apareció en el timeout configurado. En ese caso,
                //    NO interpretamos como éxito ni error; simplemente reintentamos.
                if (!found)
                {
                    outputHelper?.WriteLine("Ni éxito ni error aparecieron dentro del timeout en este intento.");
                    continue; // Reintentar hasta agotar maxRetries
                }

                // 6) Si la tarea que terminó primero devolvió 'true', chequeamos cuál era:
                if (first == successTask)
                {
                    outputHelper?.WriteLine($"Elemento de éxito encontrado: {locatorSuccess.ToString()}");
                    return; // Éxito, salimos del método
                }
                else if (first == errorTask)
                {
                    outputHelper?.WriteLine($"Elemento de error encontrado: {locatorError.ToString()}");
                    continue;
                }
            }

            throw new TimeoutException("Se ha alcanzado el numero maximo de reintentos.");
        }
        /// <summary>
        /// Click a un elemento y espera a que aparezca el elemento de confirmación. Repite si no aparece en el límite de tiempo.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="locatorClick"></param>
        /// <param name="locatorSuccess"></param>
        /// <param name="optionsWait"></param>
        /// <param name="outputHelper"></param>
        /// <param name="maxRetries"></param>
        /// <param name="optionsClick"></param>
        /// <param name="transitionBufferMs"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static async Task ClickAndWaitAsync(
            this IPage page,
            ILocator locatorClick,
            ILocator locatorSuccess,
            LocatorWaitForOptions? optionsWait = null,
            ITestOutputHelper? outputHelper = null,
            int maxRetries = 3,
            LocatorClickOptions? optionsClick = null,
            int transitionBufferMs = 500
            )
        {
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                if (attempt > 0)
                    outputHelper?.WriteLine($"Reintento #{attempt}");

                // 1) Hacer click
                await locatorClick.ClickAsync(optionsClick);

                // 2) Pequeño buffer para que el UI se estabilice
                await Task.Delay(transitionBufferMs);

                // 3) Arrancar las tarea de espera
                bool successTask = await locatorSuccess.WaitForElementSafeAsync(optionsWait);

                if (successTask)
                {
                    outputHelper?.WriteLine($"Elemento de éxito encontrado: {locatorSuccess.ToString()}");
                    return; // Éxito, salimos del método
                }
                else
                {
                    outputHelper?.WriteLine("Elemento de error encontrado.");
                }
            }

            throw new TimeoutException("Ni el elemento de éxito ni el de error aparecieron tras los reintentos.");
        }
        public static async Task<bool> WaitForElementSafeAsync(this ILocator locator, LocatorWaitForOptions? options = null)
        {
            try
            {
                await locator.WaitForAsync(options);
                return true;
            }
            catch (TimeoutException)
            {
                // No se encontró dentro del timeout
                return false;
            }
        }
        public static async Task FillIfEditableAsync(this ILocator locator,
            string value,
            ITestOutputHelper? outputHelper = null,
            LocatorFillOptions? options = null)
        {

            // Verificar si el elemento es editable antes de intentar llenar
            if (await locator.IsEditableAsync())
            {
                await locator.FillAsync(value, options);
            }
            else
            {
                outputHelper?.WriteLine($"El elemento no es editable: {locator.ToString()}");
            }
        }
    }
}

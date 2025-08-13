using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Services.ActionCoordination;

namespace AutomationTest.FitbankWeb3.Tests.CoreTest
{
    [Trait("Grupo", "CoreTest")]
    public class ActionCoordinatorServiceTests
    {
        [Fact]
        public void CreateHandle_ReturnsValidHandle()
        {
            // Arrange
            var service = new ActionCoordinatorService();

            // Act
            var handle = service.CreateHandle();

            // Assert
            Assert.NotNull(handle);
            Assert.IsAssignableFrom<IActionHandle>(handle);
        }

        [Fact]
        public async Task SingleHandle_AcquiresTurnImmediately()
        {
            // Arrange
            var service = new ActionCoordinatorService();
            using var handle = service.CreateHandle();

            // Act
            var waitTask = handle.WaitForTurnAsync();

            // Assert
            await AssertCompletesImmediately(waitTask);
        }

        [Fact]
        public async Task MultipleHandles_SecondWaitsForFirstToDispose()
        {
            // Arrange
            var service = new ActionCoordinatorService();
            using var handle1 = service.CreateHandle();
            using var handle2 = service.CreateHandle();

            // Act
            var task1 = handle1.WaitForTurnAsync();
            await AssertCompletesImmediately(task1);

            var task2 = handle2.WaitForTurnAsync();
            Assert.False(task2.IsCompleted);

            // Liberar el primer handle
            handle1.Dispose();

            // Assert
            await AssertCompletesImmediately(task2);
        }

        [Fact]
        public async Task ExceptionAfterAcquiringTurn_ReleasesSemaphoreAutomatically()
        {
            // Arrange
            var service = new ActionCoordinatorService();

            // Act - Parte 1: Simular una operación que falla después de adquirir el turno
            var firstHandle = service.CreateHandle();
            try
            {
                await firstHandle.WaitForTurnAsync();
                throw new InvalidOperationException("Simulated error during critical operation");
            }
            catch
            {
                /* Solo capturamos para continuar la prueba */
            }
            finally
            {
                // Aseguramos la liberación incluso con errores
                firstHandle.Dispose();
            }

            // Act - Parte 2: Intentar adquirir el turno con un nuevo handle
            using (var secondHandle = service.CreateHandle())
            {
                // Assert: Verificamos que podemos adquirir el turno inmediatamente
                await AssertCompletesImmediately(secondHandle.WaitForTurnAsync());
            }
        }
        [Fact]
        public async Task ConcurrentAccess_OnlyOneAtATime()
        {
            // Se busca verificar que solo hay una rama en la seccion critica a la vez
            // Se calcula la cantidad maxima de ramas en la seccion critica durante toda la ejecucion
            // Arrange
            var service = new ActionCoordinatorService();
            int concurrentCount = 0;
            int maxConcurrent = 0;
            var tasks = new Task[5];

            // Act
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    using var handle = service.CreateHandle();
                    await handle.WaitForTurnAsync();

                    // Sección crítica
                    int current = Interlocked.Increment(ref concurrentCount);
                    Interlocked.Exchange(ref maxConcurrent, Math.Max(maxConcurrent, current));
                    await Task.Delay(50); // Simular trabajo
                    Interlocked.Decrement(ref concurrentCount);
                });
            }

            // Assert
            await Task.WhenAll(tasks);
            Assert.Equal(1, maxConcurrent);
        }
        [Fact]
        public async Task DisposeWithoutWait_DoesNotBlock()
        {
            // Arrange
            var service = new ActionCoordinatorService();
            var handle1 = service.CreateHandle();
            var handle2 = service.CreateHandle();

            // Act
            handle1.Dispose(); // Dispose sin esperar

            // Assert
            await AssertCompletesImmediately(handle2.WaitForTurnAsync());
        }

        [Fact]
        public async Task MultipleDispose_IsSafe()
        {
            // Arrange
            var service = new ActionCoordinatorService();
            var handle = service.CreateHandle();

            // Act
            await handle.WaitForTurnAsync();
            handle.Dispose();

            // Assert
            handle.Dispose(); // Segundo dispose no debe lanzar excepción
            using var nextHandle = service.CreateHandle();
            await AssertCompletesImmediately(nextHandle.WaitForTurnAsync());
        }
        private async Task AssertCompletesImmediately(Task task, int timeout = 50)
        {
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout));
            Assert.Same(task, completedTask);
            await task; // Para propagar posibles excepciones
        }
    }
}

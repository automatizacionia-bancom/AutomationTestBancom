using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Services;
using Xunit.Abstractions;

namespace AutomationTest.FitbankWeb3.Tests.CoreTest
{
    [Trait("Grupo", "CoreTest")]
    public class UserTurnCoordinatorServiceTests
    {
        // private readonly IUserTurnCoordinatorService svc = new UserTurnCoordinatorService();
        private readonly ITestOutputHelper _output;

        public UserTurnCoordinatorServiceTests(ITestOutputHelper output)
        {
            _output = output;
        }
        [Fact]
        public async Task SingleBranch_AlwaysWins()
        {
            // Arrange
            var svc = new UserTurnCoordinatorService();
            var session = svc.RegisterBranch();

            // Act
            var task = session.ArriveUntilTurnAsync("user1");

            // Assert
            await task; // Debe completarse inmediatamente
            Assert.True(task.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task TwoBranches_SameUser_BothWin()
        {
            // Arrange
            var svc = new UserTurnCoordinatorService();
            var sessions = new[] {
            svc.RegisterBranch(),
            svc.RegisterBranch()
        };

            // Act
            var tasks = sessions.Select(s =>
                s.ArriveUntilTurnAsync("user1")).ToList();

            // Assert
            await Task.WhenAll(tasks);
            Assert.All(tasks, t => Assert.True(t.IsCompletedSuccessfully));
        }

        [Fact]
        public async Task ThreeBranches_DifferentUsers_OneWins()
        {
            // Arrange
            var svc = new UserTurnCoordinatorService();
            var sessions = new[] {
            svc.RegisterBranch(),
            svc.RegisterBranch(),
            svc.RegisterBranch()
        };

            var users = new[] { "user1", "user2", "user1" }; // user1 gana (2 votos)

            // Act
            var tasks = sessions.Select((s, i) =>
                s.ArriveUntilTurnAsync(users[i])).ToList();

            // Assert
            var completedTask = await Task.WhenAny(tasks);
            await completedTask; // Una tarea debe completarse (ganadora)

            // Las otras deben seguir en espera
            Assert.Equal(2, tasks.Count(t => t.IsCompleted));
            Assert.Equal(1, tasks.Count(t => !t.IsCompleted));
        }

        [Fact]
        public async Task BranchDeregistrationAfter_ShouldNotAffectWinners()
        {
            // Arrange
            var svc = new UserTurnCoordinatorService();
            var winnerSession = svc.RegisterBranch();
            var loserSession = svc.RegisterBranch();

            // Act
            var winnerTask = winnerSession.ArriveUntilTurnAsync("1_winner");
            var loserTask = loserSession.ArriveUntilTurnAsync("2_loser");

            // Esperar a que se complete la primera ronda
            await winnerTask; // winnerTask gana primero por el orden alfabetico

            // Desregistrar 
            loserSession.Dispose();

            // Nueva ronda con solo el ganador
            var newRoundTask = winnerSession.ArriveUntilTurnAsync("1_winner");

            // Assert
            await newRoundTask; // Debe completarse inmediatamente
            Assert.True(newRoundTask.IsCompletedSuccessfully);
        }
        [Fact]
        public async Task BranchDeregistrationBefore_ShouldNotAffectWinners2()
        {
            // Arrange
            var svc = new UserTurnCoordinatorService();
            var earlySession = svc.RegisterBranch();
            var lateSession = svc.RegisterBranch();

            // Act - Primera ronda solo con earlySession
            var earlyTask = earlySession.ArriveUntilTurnAsync("winner");
            lateSession.Dispose();

            // Esperar a que complete la ronda
            await earlyTask;
            Assert.True(earlyTask.IsCompletedSuccessfully);
        }
        [Fact]
        public async Task MultipleRounds_WinnerCanChange()
        {
            // Arrange
            var svc = new UserTurnCoordinatorService();
            var sessionA = svc.RegisterBranch();
            var sessionB = svc.RegisterBranch();

            // Round 1: A gana
            Task taskA1 = sessionA.ArriveUntilTurnAsync("userA");
            Task taskB1 = sessionB.ArriveUntilTurnAsync("userB"); // seguira esperando
            await taskA1; // gana por orden alfabetico

            // Round 2: B gana
            Task taskA2 = sessionA.ArriveUntilTurnAsync("userB");

            // Assert
            await Task.WhenAll(taskA2, taskB1);
            Assert.True(taskA2.IsCompletedSuccessfully);
            Assert.True(taskB1.IsCompletedSuccessfully);
        }
        [Fact]
        public async Task HighConcurrency_ShouldHandleMultipleBranches()
        {
            // Arrange
            var svc = new UserTurnCoordinatorService();
            const int branchCount = 10;
            var sessions = new List<IBranchSession>();
            var tasks = new List<Task>();

            // Act
            for (int i = 0; i < branchCount; i++)
            {
                var session = svc.RegisterBranch();
                sessions.Add(session);
            }
            for (int i = 0; i < branchCount; i++)
            {
                // Alternar usuarios: user0, user1, user0, user1...
                tasks.Add(sessions[i].ArriveUntilTurnAsync($"user{i % 2}"));
            }
            for (int i = 0; i < branchCount; i = i + 2)
            {
                // terminaran las ramas co user0
                await tasks[i];
                sessions[i].Dispose(); // desregistrar las ramas
            }
            for (int i = 1; i < branchCount; i = i + 2)
            {
                // terminaran las ramas co user1
                await tasks[i];
                sessions[i].Dispose(); // desregistrar las ramas
            }

            //// Assert
            Assert.All(tasks, t => Assert.True(t.IsCompletedSuccessfully));
        }
        [Fact]
        public async Task LateArrival_ShouldJoinNextRound()
        {
            // Arrange
            var svc = new UserTurnCoordinatorService();
            var earlySession = svc.RegisterBranch();

            // Act - Primera ronda solo con earlySession
            var earlyTask = earlySession.ArriveUntilTurnAsync("early");

            // Esperar a que complete primera ronda
            await earlyTask;

            // LateSession se une en segunda ronda
            var lateSession = svc.RegisterBranch();

            var lateTask = lateSession.ArriveUntilTurnAsync("late");
            var earlyTask2 = earlySession.ArriveUntilTurnAsync("late"); // Mismo usuario

            // Assert
            await Task.WhenAll(earlyTask2, lateTask); // Ambos deben ganar
            Assert.True(earlyTask2.IsCompletedSuccessfully);
            Assert.True(lateTask.IsCompletedSuccessfully);
        }
    }
}

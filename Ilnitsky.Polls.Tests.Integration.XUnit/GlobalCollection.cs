namespace Ilnitsky.Polls.Tests.Integration.XUnit;

[CollectionDefinition("GlobalCollection")]
public class GlobalCollection : ICollectionFixture<AppFixture>;
// Атрибут для объединения тестов в коллекцию, чтобы тесты из разных классов выполнялись последовательно,
// т.к. xUnit тесты из одной коллекции выполняет последовательно

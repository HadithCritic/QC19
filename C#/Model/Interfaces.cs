using System.IO;

public enum Subject { LanguageSystem, SimplificationSystem, NumericalSystem, DNASequenceSystem, InterestingNumbers };

public interface IPublisher
{
    void Subscribe(Subject subject, ISubscriber subscriber);
    void Unsubscribe(Subject subject, ISubscriber subscriber);
}

public interface ISubscriber
{
    void Notify(Subject subject, FileSystemEventArgs e);
}

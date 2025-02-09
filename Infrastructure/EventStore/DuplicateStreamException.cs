namespace Infrastructure.EventStore;

public class DuplicateStreamException(string streamName) : Exception($"Stream {streamName} already exists");
namespace Cabinet.Core {
    public interface IWriteProgress {
        long BytesWritten { get; }
        long? TotalBytes { get; }
    }
}
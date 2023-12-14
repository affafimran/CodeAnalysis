namespace _360Fabriek {
    public enum State {
        Uninitialised = 0x01,
        Inactive = 0x02,
        Active = 0x04,
        Finished = 0x08,
        Halted = 0x16,
        Invalid = 0x32,
        Skipped = 0x64
    }
}
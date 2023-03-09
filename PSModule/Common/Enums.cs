
namespace PSModule
{
    public enum RunType
    {
        Alm,
        AlmLabManagement,
        FileSystem,
        LoadRunner
    }

    public enum AlmRunMode
    {
        RUN_NONE,
        RUN_LOCAL,
        RUN_REMOTE,
        RUN_PLANNED_HOST
    }

    public enum RunTestType
    {
        TEST_SUITE,
        BUILD_VERIFICATION_SUITE
    }

    public enum ArtifactType
    {
        onlyReport,
        onlyArchive,
        bothReportArchive,
        None
    }

    public enum RunStatus
    {
        PASSED = 0,
        FAILED = -1,
        UNSTABLE = -2,
        CANCELED = -3,
        UNDEFINED = -9
    }
    public enum LauncherExitCode
    {
        Passed = 0,
        Failed = -1,
        PartialFailed = -2,
        Aborted = -3,
        Unstable = -4,
        AlmNotConnected = -5,
        Closed = -1073741510 // 0xC000013A STATUS_CONTROL_C_EXIT https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-erref/596a1078-e883-4972-9bbc-49e60bebca55
    }

    public enum EnvType
    {
        None,
        Mobile,
        Web
    }
}

namespace TA.AssetValidation
{
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }

    public enum AssetIssueType
    {
        None,
        Naming,
        Path,
        TextureSize,
        TextureType,
        ModelScale,
        ReadWrite,
        MissingMaterial,
        MissingReference,
        MissingScript,
        PrefabStructure,
        ShaderMissing,
        ImportSetting
    }
}

namespace Bonsai.Code.DomainModel.Facts.Templates
{
    /// <summary>
    /// The template for specifying blood type.
    /// </summary>
    public class BloodTypeFactTemplate: IFactTemplate
    {
        public BloodType Type { get; set; }

        public bool RhesusFactor { get; set; }
    }

    public enum BloodType
    {
        O,
        A,
        B,
        AB
    }
}

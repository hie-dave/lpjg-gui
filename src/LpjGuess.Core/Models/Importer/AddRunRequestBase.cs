using System.ComponentModel.DataAnnotations;

namespace LpjGuess.Core.Models.Importer;

/// <summary>
/// Base request model for adding a run to any type of dataset.
/// </summary>
public abstract class AddRunRequestBase
{
    /// <summary>
    /// Compressed instruction file for running the model.
    /// </summary>
    [Required]
    public byte[] InstructionFile { get; set; } = Array.Empty<byte>();
}

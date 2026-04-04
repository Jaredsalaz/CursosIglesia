namespace CursosIglesia.Models;

public class QuizAttempt
{
    public Guid IdUsuario { get; set; }
    public Guid IdQuiz { get; set; }
    public double PuntajeObtenido { get; set; }
    public int MinimoRequerido { get; set; }
    public DateTime FechaIntento { get; set; } = DateTime.UtcNow;
}

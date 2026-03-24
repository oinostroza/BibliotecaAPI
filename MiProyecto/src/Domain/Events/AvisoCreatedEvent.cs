namespace MiProyecto.Domain.Events;

public class AvisoCreatedEvent : BaseEvent
{
    public AvisoCreatedEvent(Aviso aviso)
    {
        Aviso = aviso;
    }

    public Aviso Aviso { get; }
}

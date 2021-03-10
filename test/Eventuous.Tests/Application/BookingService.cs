using Eventuous.Tests.Model;

namespace Eventuous.Tests.Application {
    public class BookingService : ApplicationService<Booking, BookingState, BookingId> {
        public BookingService(IAggregateStore store) : base(store) {
            OnNew<Commands.BookRoom>(
                (booking, cmd)
                    => booking.BookRoom(
                        new BookingId(cmd.BookingId),
                        cmd.RoomId,
                        new StayPeriod(cmd.CheckIn, cmd.CheckOut),
                        cmd.Price
                    )
            );
        }
    }
}
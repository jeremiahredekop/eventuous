// Copyright (C) Ubiquitous AS. All rights reserved
// Licensed under the Apache License, Version 2.0.

using Eventuous.Sut.Domain;
using static Eventuous.Sut.App.Commands;
using static Eventuous.Sut.Domain.BookingEvents;

namespace Eventuous.Tests.Application;

public class BookingFuncService : FunctionalCommandService<BookingState> {
    public BookingFuncService(IEventStore store, TypeMapper? typeMap = null)
        : base(store, typeMap) {
#pragma warning disable CS0618 // Type or member is obsolete
        OnNew<BookRoom>(cmd => GetStream(cmd.BookingId), BookRoom);
#pragma warning restore CS0618 // Type or member is obsolete
        On<RecordPayment>().InState(ExpectedState.Existing).GetStream(cmd => GetStream(cmd.BookingId)).Act(RecordPayment);

        return;

        static StreamName GetStream(string id)
            => StreamName.For<Booking>(id);

        static IEnumerable<object> BookRoom(BookRoom cmd) {
            yield return new RoomBooked(cmd.RoomId, cmd.CheckIn, cmd.CheckOut, cmd.Price);
        }

        static IEnumerable<object> RecordPayment(BookingState state, object[] originalEvents, RecordPayment cmd) {
            if (state.HasPayment(cmd.PaymentId)) yield break;

            var registered = new BookingPaymentRegistered(cmd.PaymentId, cmd.Amount.Amount);

            yield return registered;

            var newState = state.When(registered);

            if (newState.IsFullyPaid()) yield return new BookingFullyPaid(cmd.PaidAt);
            if (newState.IsOverpaid()) yield return new BookingOverpaid((state.AmountPaid - state.Price).Amount);
        }
    }
}

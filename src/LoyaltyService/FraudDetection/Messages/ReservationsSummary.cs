namespace LoyaltyService.FraudDetection.Messages
{
	public class ReservationsSummary
	{
		public long NumberOfReservationsInLastMonth { get; set; }
		public long NumberOfReservations { get; set; }
		public long NumberOfPendingReservations { get; set; }
		public long NumberOfSeatedReservations { get; set; }
		public long NumberOfAssumedSeatedReservations { get; set; }
		public long NumberOfDisputedSeatedReservations { get; set; }
		public long NumberOfNoShowReservations { get; set; }
		public long NumberOfNoShowExcusedReservations { get; set; }
		public long NumberOfCanceledWebReservations { get; set; }
		public long NumberOfCanceledRestaurantReservations { get; set; }
		public long NumberOfCanceledDisputedReservations { get; set; }
	}
}

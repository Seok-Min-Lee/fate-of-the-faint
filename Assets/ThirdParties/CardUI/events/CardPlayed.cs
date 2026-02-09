namespace events {

    public class CardPlayed : CardEvent {
        public CardPlayed(CardView card) : base(card) {
        }
    }
}

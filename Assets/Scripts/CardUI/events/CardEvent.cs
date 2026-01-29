namespace events {
    
    public class CardEvent {
        public readonly CardView card;

        public CardEvent(CardView card) {
            this.card = card;
        }
    }
}

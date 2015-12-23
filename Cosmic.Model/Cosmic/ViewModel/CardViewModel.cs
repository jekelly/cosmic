using System.Collections.ObjectModel;
using Cosmic.Model;
using GalaSoft.MvvmLight;

namespace Cosmic.ViewModel
{
	class PlayerViewModel : ViewModelBase
	{
		public PlayerViewModel()
		{
			this.Hand = new ObservableCollection<CardViewModel>();
		}

		public ObservableCollection<CardViewModel> Hand { get; private set; }
	}

	class CardViewModel : ViewModelBase
	{
		private readonly ICard card;

		public CardViewModel(ICard card)
		{
			this.card = card;
		}
	}

	//class NegotiateCardViewModel : CardViewModel
	//{
	//}

	//class AttackCardViewModel : CardViewModel
	//{
	//	private readonly int value;

	//	public AttackCardViewModel()
	//	{
	//		this.value = 40;
	//	}

	//	public int Value { get { return this.value; } }
	//}
}

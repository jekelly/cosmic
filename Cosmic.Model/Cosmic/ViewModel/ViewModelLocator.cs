using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleInjector;

namespace Cosmic.ViewModel
{
	class ViewModelLocator
	{
		private readonly Container container;

		public ViewModelLocator()
		{
			this.container = new Container();
			this.container.Register<PlayerViewModel>();
		}
	}
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Gold_Client.ViewModel.Tests
{
    [TestClass()]
    public class MainContentPresenterTests
    {
        static Action mainContentCloseAction;
        MainContentPresenter mainContentPresenter = new MainContentPresenter(mainContentCloseAction);

        [TestMethod()]
        public void AddFriendHandleCommandTest()
        {
            //Assert.IsFalse(mainContentPresenter.PrivateMsgToUserCommand.CanExecute(null));
            //ObservableCollection<string> usersConnected = new ObservableCollection<string>();
            //usersConnected.Add("User");
            //Assert.IsTrue(mainContentPresenter.PrivateMsgToUserCommand.CanExecute(null));
            //Assert.AreEqual(usersConnected.First(), "User");
        }
    }
}
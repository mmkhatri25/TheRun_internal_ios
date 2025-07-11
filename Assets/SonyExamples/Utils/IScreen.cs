using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public interface IScreen
{
#if UNITY_PS4
	void Process(MenuStack stack);
	void OnEnter();
	void OnExit();
#endif
}


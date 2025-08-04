public class SingleReadParam<T>
{
	private T param_;

	public bool isSet
	{
		get;
		protected set;
	}

	public T param
	{
		get
		{
			isSet = false;
			return param_;
		}
		set
		{
			isSet = true;
			param_ = value;
		}
	}
}

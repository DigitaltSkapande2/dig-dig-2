using System;

public struct VoteFlag
{
	public bool IsActive => votes > 0;
	private int votes;

	public void AddVote()
	{
		votes++;
	}

	public void RemoveVote()
	{
		votes--;
	}

	public static implicit operator bool(VoteFlag flag)
	{
		return flag.IsActive;
	}
}
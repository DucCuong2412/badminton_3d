using ProtoModels;
using System.Collections.Generic;

public class OwnedItems
{
	public static string DefaultOwnedItemsFilename = "ownedItems.bytes";

	private OwnedItemsDAO ownedItems;

	public string OwnedItemsFilename
	{
		get;
		private set;
	}

	public OwnedItems(string filename)
	{
		OwnedItemsFilename = filename;
		if (!ProtoIO.LoadFromFileCloudSync(OwnedItemsFilename, out ownedItems))
		{
			ownedItems = new OwnedItemsDAO();
			Save();
		}
		if (ownedItems.ownedItems == null)
		{
			ownedItems.ownedItems = new List<OwnedItemDAO>();
			Save();
		}
	}

	public void ResolvePotentialConflictsWithCloudData()
	{
		GGFileIOCloudSync instance = GGFileIOCloudSync.instance;
		if (!instance.isInConflict(OwnedItemsFilename))
		{
			return;
		}
		GGFileIO cloudFileIO = instance.GetCloudFileIO();
		OwnedItemsDAO model;
		if (ProtoIO.LoadFromFile<ProtoSerializer, OwnedItemsDAO>(OwnedItemsFilename, cloudFileIO, out model) && model != null && model.ownedItems != null && model.ownedItems.Count != 0)
		{
			if (ownedItems.ownedItems == null)
			{
				ownedItems.ownedItems = new List<OwnedItemDAO>();
			}
			foreach (OwnedItemDAO ownedItem in model.ownedItems)
			{
				if (!isOwned(ownedItem.name))
				{
					addToOwned(ownedItem.name);
				}
			}
			ProtoIO.SaveToFile<ProtoSerializer, OwnedItemsDAO>(OwnedItemsFilename, cloudFileIO, ownedItems);
		}
	}

	public void addToOwned(string name)
	{
		if (!isOwned(name))
		{
			OwnedItemDAO ownedItemDAO = new OwnedItemDAO();
			ownedItemDAO.name = name;
			ownedItems.ownedItems.Add(ownedItemDAO);
			Save();
		}
	}

	public bool isOwned(string name)
	{
		OwnedItemDAO ownedItemDAO = ownedItems.ownedItems.Find((OwnedItemDAO o) => o.name == name);
		return ownedItemDAO != null;
	}

	public int maxOwnedIndexOf(string name, int count)
	{
		int result = -1;
		for (int i = 0; i < count; i++)
		{
			if (isOwned(name + i))
			{
				result = i;
			}
		}
		return result;
	}

	public void addIndexedItemToOwned(string name, int index)
	{
		addToOwned(name + index);
	}

	public void Save()
	{
		ProtoIO.SaveToFileCloudSync(OwnedItemsFilename, ownedItems);
	}
}

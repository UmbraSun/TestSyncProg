﻿namespace TestSyncProg.Entity;

public class MaterialMSSql
{
    public int Id { get; set; }

    public string Name { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime LastUpdate { get; set; }
}
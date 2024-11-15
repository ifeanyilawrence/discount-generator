﻿namespace DiscountGenerator.Domain;

public class PageInfo
{
    public PageInfo(
        int pageNumber, 
        int pageSize, 
        int totalItems, 
        int totalPages)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalItems = totalItems;
        TotalPages = totalPages;
    }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalItems { get; set; }

    public int TotalPages { get; set; }
}
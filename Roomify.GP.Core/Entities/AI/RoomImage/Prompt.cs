using Roomify.GP.Core.Entities.AI.RoomImage;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Roomify.GP.Core.Entities.AI.RoomImage
{
    public class Prompt
    {
        public Guid Id { get; set; }
        public string DescriptionText { get; set; }
        public RoomStyle RoomStyle { get; set; }  // Enum
        public RoomType RoomType { get; set; }    // Enum



        // Either RoomImageId or AIResultHistoryId should be set (not both)
        public Guid? RoomImageId { get; set; }
        [ForeignKey("RoomImageId")]
        public virtual RoomImage RoomImage { get; set; }

        public Guid? AIResultHistoryId { get; set; }
        [ForeignKey("AIResultHistoryId")]
        public virtual AIResultHistory AIResultHistory { get; set; }
    }
}
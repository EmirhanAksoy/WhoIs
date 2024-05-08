import { Component, OnInit } from '@angular/core';
import { ImageService } from 'src/app/services/image.service';
import { debounceTime } from 'rxjs/operators';
import { FormControl } from '@angular/forms';

@Component({
  selector: 'app-image-search',
  templateUrl: './image-search.component.html',
  styleUrls: ['./image-search.component.scss']
})
export class ImageSearchComponent implements OnInit {
  imageIds: string[] = [];
  searchControl = new FormControl<string>('');
  constructor(private imageService: ImageService) { }

  ngOnInit(): void {
    this.searchControl.valueChanges.pipe(
      debounceTime(500)
    ).subscribe(searchTerm => {
      if (!searchTerm) {
        this.imageIds = [];
        return;
      }
      this.getImagePaths(searchTerm);
    });
  }

  getImagePaths(searchText: string) {
    this.imageService.getImagePathsWithFaceName(searchText).subscribe(
      apiResponse => {
        if (apiResponse.success) {
          this.imageIds = apiResponse?.data || [];
        } else {
          console.error(apiResponse);
        }
      }, console.error
    );
  }
}

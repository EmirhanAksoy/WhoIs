import { Component, Input, OnInit } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { ImageService } from '../../services/image.service';

@Component({
  selector: 'app-face-image',
  templateUrl: './face-image.component.html',
  styleUrls: ['./face-image.component.scss']
})
export class FaceImageComponent implements OnInit {

  @Input() imageId: string = '';
  @Input() imageName: string = '';
  @Input() isFaceImage: boolean = true;

  imageUrl: SafeUrl | string = '';
  constructor(private imageService: ImageService,
    private sanitizer: DomSanitizer
  ) { }
  ngOnInit(): void {
    this.getFaceImage();
  }

  getFaceImage(): void {
    const urlCreator = window.URL;
    this.imageService.getImageAsBlob(this.imageId, this.isFaceImage).subscribe(response => {
      const blob = new Blob([response.body!], { type: response.headers.get('Content-Type')! });
      this.imageUrl = this.sanitizer.bypassSecurityTrustUrl(urlCreator.createObjectURL(blob))
    })
  }

  editFaceName(): void {
    const faceName = prompt('Enter face name', '');
    if (!faceName) {
      prompt('Face name cannot be empty.')
      return;
    }
    this.imageService.updateFaceName(this.imageId, faceName).subscribe(data => {
      this.imageName = faceName;
      alert('Face name updated successfully.')
    }, error => {
      alert('An error occured while updating face name');
      console.error(error);
    });
  }

}

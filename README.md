# Deformable Unity Script

This Unity script, titled "Deformable," facilitates dynamic deformation of a 3D object based on collisions. The script provides configurable parameters for controlling the extent and nature of deformations, allowing users to achieve realistic effects in their projects.

## Features

- **Deformation Types**: The script supports two types of deformation: "Deformar" and "DeformarYBorrar," allowing for different responses to collisions.
  
- **Deformer Object Types**: Users can choose between a static or dynamic deformer object, determining whether the object causing deformation has velocity or is static/moved artificially.

- **Configurable Parameters**: Users can fine-tune various parameters such as minimum impulse, malleability, maximum impulse, impact zone, and the type of deterioration (deform only or deform and erase).

## Usage

1. Attach the script to a GameObject with a MeshFilter component.
2. Ensure the GameObject has a Rigidbody and MeshCollider component, as specified by [RequireComponent] attributes in the script.
3. Configure the deformation parameters in the Inspector to achieve the desired deformation behavior.
4. Collisions with the assigned Collider trigger the deformation effect.

## Additional Notes

- The script uses inverse transforms to work in local space, allowing for consistent calculations regardless of the object's orientation.
- Deformation is based on collision details such as the collision point, impulse, and normals.
- The script provides options for different responses to collisions, including simple deformation or deformation and erasing triangles.
- The mesh updates dynamically during runtime, recalculating vertices, triangles, normals, and bounds.

Feel free to explore and modify this script to suit your specific project requirements. If you encounter any issues or have questions, please refer to the accompanying documentation or contact the script author.

## Author

Txomin Errasti

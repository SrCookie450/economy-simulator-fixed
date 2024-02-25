export class Command {
    command: string;
    args: any;
    id: number;
}

export class AvatarRenderRequest {
    userId: number;
    playerAvatarType: 'R6' | 'R15';
    scales: any;
    bodyColors: {
        headColorId: number;
        torsoColorId: number;
        rightArmColorId: number;
        leftArmColorId: number;
        rightLegColorId: number;
        leftLegColorId: number;
    };
    assets: [
        {
            id: number;
        }
    ];
}